export interface RequestState<T> {
  status: 'loading' | 'loaded' | 'error';
  data?: T;
  errorMessage?: string;
}
